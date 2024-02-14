import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ssm from 'aws-cdk-lib/aws-ssm';

export interface IStackSettings extends cdk.StackProps {
  action: string;
  accountId: string;
  service_key: string;
  target_environment: string;
  vpc_lambda_cidr: string;
  vpc_api_cidr: string;
  buildType: string;
}

export class ServiceCdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: IStackSettings) {
    super(scope, id, props);

    this.createLambdaVpc(props);
    //this.createApiVpc(props);
  }

  private createLambdaVpc(props: IStackSettings) {
    // Create a VPC for the Lambda services
    const vpc = new ec2.Vpc(
      this,
      `lambda-vpc-alt-${props.target_environment}`,
      {
        cidr: props.vpc_lambda_cidr,
        natGateways: 1,
        subnetConfiguration: [
          { cidrMask: 24, subnetType: ec2.SubnetType.PUBLIC, name: 'Public' },
          {
            cidrMask: 24,
            subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
            name: 'Private',
          },
        ],
        maxAzs: 2,
      }
    );

    // save vpcId to SSM parameter
    new ssm.StringParameter(this, 'ssm-vpcId', {
      parameterName: `/network/${props.target_environment}/vpcId`,
      stringValue: vpc.vpcId,
    });

    // save privateSubnetIds to SSM parameter
    const privateSubnetIds = vpc.privateSubnets.map((sn, i) => {
      return sn.subnetId;
    });

    new ssm.StringParameter(this, 'ssm-privateSubnetIds', {
      parameterName: `/network/${props.target_environment}/privateSubnetIds`,
      stringValue: JSON.stringify(privateSubnetIds),
    });

    // Egress SG for outbound traffic --> SQL DB Azure
    const egressSg = new ec2.SecurityGroup(this, 'LambdaEgressSG', {
      securityGroupName: `lambda-egress-sg-alt-${props.target_environment}`,
      vpc: vpc,
    });

    egressSg.addEgressRule(ec2.Peer.ipv4('0.0.0.0/0'), ec2.Port.tcp(443));

    // save egressSecurityGroupId to SSM parameter
    new ssm.StringParameter(this, 'ssm-egressSecurityGroupId', {
      parameterName: `/network/${props.target_environment}/egressSecurityGroupId`,
      stringValue: egressSg.securityGroupId,
    });

    // VPE SG for AWS traffic
    const vpeSg = new ec2.SecurityGroup(this, 'LambdaVpeSG', {
      securityGroupName: `lambda-vpe-sg-alt-${props.target_environment}`,
      vpc: vpc,
    });

    vpeSg.addEgressRule(ec2.Peer.ipv4('0.0.0.0/0'), ec2.Port.tcp(443));

    // save vpeSecurityGroupId to SSM parameter
    new ssm.StringParameter(this, 'ssm-vpeSecurityGroupId', {
      parameterName: `/network/${props.target_environment}/vpeSecurityGroupId`,
      stringValue: vpeSg.securityGroupId,
    });

    // VPC Endpoint to access Secrets Manager
    const secretsManagerEndpoint = new ec2.InterfaceVpcEndpoint(
      this,
      'LambdaSecretsManagerEndpoint',
      {
        vpc,
        service: new ec2.InterfaceVpcEndpointService(
          'com.amazonaws.ap-southeast-2.secretsmanager',
          443
        ),
        securityGroups: [vpeSg],
      }
    );

    new cdk.CfnOutput(this, 'vpcId', { value: vpc.vpcId });
    new cdk.CfnOutput(this, 'privateSubnetIds', {
      value: JSON.stringify(privateSubnetIds),
    });
    new cdk.CfnOutput(this, 'egressSecurityGroupId', {
      value: egressSg.securityGroupId,
    });
    new cdk.CfnOutput(this, 'vpeSecurityGroupId', {
      value: vpeSg.securityGroupId,
    });
  }
}
