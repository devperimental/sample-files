import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as ecr from 'aws-cdk-lib/aws-ecr';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import { Duration, Tags } from 'aws-cdk-lib';

export interface IFargateSettings extends cdk.StackProps {
  targetEnvironment: string;
  aspNetEnvironment: string;
  serviceKey: string;
  serviceName: string;
  layer: string;
  awsRegion: string;
  hostEnvironmentType: string;
  applicationName: string;
  ecr_repository_name: string;
  build_run_number: string;
}

export class FargateServiceConstruct extends Construct {
  public readonly fargateService: ecs.FargateService;

  constructor(scope: Construct, id: string, props: IFargateSettings) {
    super(scope, id);

    const vpc = ec2.Vpc.fromVpcAttributes(this, 'api-vpc-dev', {
      vpcId: 'vpc-0b423d8f8f7b09c6e',
      availabilityZones: ['ap-southeast-2a', 'ap-southeast-2b'],
      privateSubnetIds: [
        'subnet-0b8b04e4f919c7e75',
        'subnet-0753d963273f23a87',
      ],
      publicSubnetIds: ['subnet-08df560b155a49b2f', 'subnet-0442c5c6cb8d6d49c'],
    });

    const defaultSg = ec2.SecurityGroup.fromSecurityGroupId(
      this,
      'api-default-sg',
      'sg-035d3ad686aeb8694'
    );

    const egressSg = ec2.SecurityGroup.fromSecurityGroupId(
      this,
      'api-egress-sg',
      'sg-0e0e9eeb085890043'
    );

    const vpeSg = ec2.SecurityGroup.fromSecurityGroupId(
      this,
      'api-vpe-sg',
      'sg-012ecbce2a95ce626'
    );

    // Create an ECS cluster
    const cluster = new ecs.Cluster(this, 'MyCluster', {
      clusterName: `${props.targetEnvironment}-${props.serviceKey}-cluster`,
      vpc: vpc,
    });

    // Execution Role Policy for the Task Definition
    const executionRolePolicy = new iam.PolicyStatement({
      effect: iam.Effect.ALLOW,
      resources: ['*'],
      actions: [
        'ecr:GetAuthorizationToken',
        'ecr:BatchCheckLayerAvailability',
        'ecr:GetDownloadUrlForLayer',
        'ecr:BatchGetImage',
        'logs:CreateLogStream',
        'logs:PutLogEvents',
        'cloudwatch:PutMetricData',
      ],
    });

    // Create a Fargate Task Definition
    const fargateTaskDefinition = new ecs.FargateTaskDefinition(
      this,
      'ApiTaskDefinition',
      {
        family: `${props.targetEnvironment}-${props.serviceKey}-taskDef`,
        memoryLimitMiB: 512,
        cpu: 256,
      }
    );

    // Attach role policies
    fargateTaskDefinition.addToExecutionRolePolicy(executionRolePolicy);

    // Get the Repository for the required service
    const repository = ecr.Repository.fromRepositoryName(
      this,
      'repo',
      props.ecr_repository_name
    );

    // Create a log group for the container
    const logGroup = new logs.LogGroup(this, 'LogGroup', {
      logGroupName: `/ecs/${props.targetEnvironment}/${props.serviceKey}`,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      retention: logs.RetentionDays.ONE_WEEK,
    });

    // Add container to the fargate task definition
    const container = fargateTaskDefinition.addContainer('backend', {
      // Use an image from Amazon ECR
      image: ecs.ContainerImage.fromEcrRepository(
        repository,
        props.targetEnvironment
      ),
      // Setup cloudwatch logs
      logging: ecs.LogDrivers.awsLogs({
        logGroup,
        streamPrefix: `${props.serviceKey}`,
        mode: ecs.AwsLogDriverMode.NON_BLOCKING,
      }), // 'signup-validator-api'
      // Pass environment variables
      environment: {
        ENVIRONMENT_NAME: `${props.targetEnvironment}`,
        ASPNETCORE_ENVIRONMENT: `${props.aspNetEnvironment}`,
        SERVICE_NAME: `${props.serviceName}`,
        LAYER: `${props.layer}`,
        AWS_REGION: `${props.awsRegion}`,
        HOST_ENVIRONMENT_TYPE: 'ECS',
        APPLICATION_NAME: `${props.applicationName}`,
      },
    });

    // Add container port mappings
    container.addPortMappings({
      containerPort: 80,
    });

    // Create a Fargate Service
    this.fargateService = new ecs.FargateService(this, 'Service', {
      serviceName: `${props.targetEnvironment}-${props.serviceKey}-service`,
      cluster,
      taskDefinition: fargateTaskDefinition,
      desiredCount: 2,
      assignPublicIp: false,
      securityGroups: [defaultSg, egressSg, vpeSg],
    });

    // Setup AutoScaling policy for the Fargate Service
    const scaling = this.fargateService.autoScaleTaskCount({
      maxCapacity: 2,
      minCapacity: 1,
    });
    scaling.scaleOnCpuUtilization('CpuScaling', {
      targetUtilizationPercent: 50,
      scaleInCooldown: Duration.seconds(60),
      scaleOutCooldown: Duration.seconds(60),
    });

    this.fargateService.taskDefinition.taskRole.addToPrincipalPolicy(
      new iam.PolicyStatement({
        effect: iam.Effect.ALLOW,
        actions: [
          'secretsManager:DescribeSecret',
          'secretsManager:GetSecretValue',
          'secretsManager:CreateSecret',
          'secretsManager:UpdateSecret',
          'secretsManager:DeleteSecret',
          'secretsManager:PutSecretValue',
          's3:PutObject',
          's3:GetObject',
          's3:DeleteObject',
        ],
        resources: ['*'],
      })
    );

    // Create Load Balancer
    const lb = new elbv2.ApplicationLoadBalancer(this, 'ALB', {
      loadBalancerName: `${props.targetEnvironment}-${props.serviceKey}-alb`,
      vpc,
      internetFacing: true,
    });

    // Add Listener to the Load Balancer
    const listener = lb.addListener('Listener', {
      port: 80,
    });

    listener.addTargets('Target', {
      port: 80,
      targets: [this.fargateService],
      healthCheck: { path: '/healthz' }, // defined in the webapi
    });

    listener.connections.allowDefaultPortFromAnyIpv4('Open to the world');

    Tags.of(this).add('build_run_number', `${props.build_run_number}`);
  }
}
