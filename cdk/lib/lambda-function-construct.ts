import * as cdk from 'aws-cdk-lib';
import { CfnOutput } from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { RuntimeFamily, Runtime } from 'aws-cdk-lib/aws-lambda';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as lambdaEventSources from 'aws-cdk-lib/aws-lambda-event-sources';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as path from 'path';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import { Queue } from 'aws-cdk-lib/aws-sqs';
import { ILambdaSettings } from './service-types';
import * as iam from 'aws-cdk-lib/aws-iam';

export class LambdaFunctionConstruct extends Construct {
  public readonly lambdaFunction: lambda.Function;
  public readonly primaryQueue?: Queue;
  public readonly functionUrl?: string;

  constructor(scope: Construct, id: string, props: ILambdaSettings) {
    super(scope, id);

    // VPC settings
    const vpc = ec2.Vpc.fromVpcAttributes(this, 'lambda-vpc-dev', {
      vpcId: 'vpc-0beebe340fe404572',
      availabilityZones: ['ap-southeast-2a', 'ap-southeast-2b'],
      privateSubnetIds: [
        'subnet-0c94ad82febafd4a6',
        'subnet-0f670ae640d7ce5ad',
      ],
    });

    const egressSg = ec2.SecurityGroup.fromSecurityGroupId(
      this,
      'lambda-egress-sg',
      'sg-0c32437a66bfaeeeb'
    );

    const vpeSg = ec2.SecurityGroup.fromSecurityGroupId(
      this,
      'lambda-vpe-sg',
      'sg-0140632bca8580449'
    );

    const functionName = props.functionName;
    const localEnvironment = props.targetEnvironment;

    const serviceDirectory = path.join(
      __dirname,
      '../..',
      props.serviceDirectory
    ); //'src/code');
    const functionBinFolder = path.join(
      serviceDirectory,
      props.lambdaProjectName,
      `bin/${props.buildType}/net6.0`
    );

    const handler = props.isWebApi
      ? props.lambdaProjectName
      : `${props.lambdaProjectName}::${props.lambdaProjectName}.Function::FunctionHandler`;

    const defaultSettings = {
      ENVIRONMENT_NAME: `${localEnvironment}`,
      ASPNETCORE_ENVIRONMENT: `${props.aspNetCoreEnvironment}`,
      SERVICE_NAME: `${props.lambdaProjectName}`,
      LAYER: 'service',
      HOST_ENVIRONMENT_TYPE: 'LAMBDA',
      APPLICATION_NAME: `${props.lambdaProjectName}`,
    };

    const lambdaSettings = { ...defaultSettings, ...props.lambdaSettings };

    this.lambdaFunction = new lambda.Function(this, `${functionName}-lambda`, {
      functionName,
      runtime: new Runtime('dotnet6', RuntimeFamily.DOTNET_CORE),
      code: lambda.Code.fromAsset(functionBinFolder),
      handler: handler,
      architecture: lambda.Architecture.X86_64,
      memorySize: 512,
      timeout: cdk.Duration.seconds(props.lambdaTimeoutSeconds),
      tracing: lambda.Tracing.ACTIVE,
      logRetention: logs.RetentionDays.TWO_WEEKS,
      environment: lambdaSettings,
      securityGroups: [egressSg, vpeSg],
      vpc: vpc,
    });

    if (props.createFunctionUrl) {
      // 👇 Setup lambda url
      const lambdaUrl = this.lambdaFunction.addFunctionUrl({
        authType: lambda.FunctionUrlAuthType.NONE,
      });

      this.functionUrl = lambdaUrl.url;
      // 👇 print lambda url after deployment
      new CfnOutput(this, 'FunctionUrl ', { value: lambdaUrl.url });
    }

    if (props.isQueueTrigger) {
      // Create Queue
      const primaryQueueName = `${functionName}-q`;
      const deadLetterQueueName = `${functionName}-dlq`;

      const deadLetterQueue = new Queue(this, 'dlq', {
        queueName: deadLetterQueueName,
      });

      this.primaryQueue = new Queue(this, 'q', {
        queueName: primaryQueueName,
        deadLetterQueue: {
          queue: deadLetterQueue,
          maxReceiveCount: 3,
        },
      });

      new CfnOutput(this, 'queue-information', {
        value: this.primaryQueue.queueArn,
        exportName: `queue-information::${primaryQueueName}::arn`,
      });

      const eventSource = new lambdaEventSources.SqsEventSource(
        this.primaryQueue
      );
      this.lambdaFunction.addEventSource(eventSource);
    }

    this.lambdaFunction.addToRolePolicy(
      new iam.PolicyStatement({
        effect: iam.Effect.ALLOW,
        actions: [
          'logs:DescribeLogGroups',
          'logs:StartQuery',
          'logs:GetQueryResults',
          'logs:GetLogEvents',
          'logs:CreateLogStream',
        ],
        resources: ['*'],
      })
    );
  }
}
