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
import * as kms from 'aws-cdk-lib/aws-kms';
import * as sns from 'aws-cdk-lib/aws-sns';
import { PolicyStatement, Effect } from 'aws-cdk-lib/aws-iam';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as subscriptions from 'aws-cdk-lib/aws-sns-subscriptions';
import * as events from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';
import { ILambdaSettings } from './service-types';

export class LambdaFunctionConstruct extends Construct {
  public readonly lambdaFunction: lambda.Function;

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

    const serviceDirectory = path.join(__dirname, '../..', 'src/code');
    const functionBinFolder = path.join(
      serviceDirectory,
      props.lambdaProjectName,
      `bin/${props.buildType}/net6.0`
    );

    const handler = props.isWebApi
      ? props.lambdaProjectName
      : `${props.lambdaProjectName}::${props.lambdaProjectName}.Function::FunctionHandler`;

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
      environment: {
        ENVIRONMENT_NAME: `${localEnvironment}`,
        ASPNETCORE_ENVIRONMENT: `${props.aspNetCoreEnvironment}`,
        SERVICE_NAME: `${props.lambdaProjectName}`,
        LAYER: 'service',
        HOST_ENVIRONMENT_TYPE: 'LAMBDA',
        APPLICATION_NAME: `${props.lambdaProjectName}`,
      },
      securityGroups: [egressSg, vpeSg],
      vpc: vpc,
    });

    if (props.createFunctionUrl) {
      // 👇 Setup lambda url
      const lambdaUrl = this.lambdaFunction.addFunctionUrl({
        authType: lambda.FunctionUrlAuthType.NONE,
      });

      // 👇 print lambda url after deployment
      new CfnOutput(this, 'FunctionUrl ', { value: lambdaUrl.url });
    }

    if (props.isQueueTrigger) {
      // Create Queue
      const primaryQueueName = `${functionName}-pq`;
      const deadLetterQueueName = `${functionName}-dlq`;

      const deadLetterQueue = new Queue(this, 'dlq', {
        queueName: deadLetterQueueName,
      });

      const primaryQueue = new Queue(this, 'pq', {
        queueName: primaryQueueName,
        deadLetterQueue: {
          queue: deadLetterQueue,
          maxReceiveCount: 3,
        },
      });

      new CfnOutput(this, 'queue-information', {
        value: primaryQueue.queueArn,
        exportName: `queue-information::${primaryQueueName}::arn`,
      });

      const eventSource = new lambdaEventSources.SqsEventSource(primaryQueue);
      this.lambdaFunction.addEventSource(eventSource);
    }

    if (props.isSnsTrigger) {
      // Create KMS Key
      const kmsKey = new kms.Key(this, `${functionName}-kms-key`, {
        removalPolicy: cdk.RemovalPolicy.DESTROY,
        pendingWindow: cdk.Duration.days(7),
        alias: `${functionName.toLowerCase()}/key-${localEnvironment}`,
        description: 'kms key for encrypting the data in sns topic',
        enableKeyRotation: true,
      });

      // Update policy to allow access from eventbridge
      kmsKey.addToResourcePolicy(
        new PolicyStatement({
          effect: Effect.ALLOW,
          principals: [new iam.ServicePrincipal('events.amazonaws.com')],
          actions: ['kms:Decrypt', 'kms:GenerateDataKey'],
          resources: ['*'],
        })
      );

      // Create Topic
      const topicName = `${functionName}-topic`;

      const topic = new sns.Topic(this, `${functionName}-topic`, {
        displayName: topicName,
        topicName: topicName,
        masterKey: kmsKey,
      });

      // Subscribe Lambda to the topic
      topic.addSubscription(
        new subscriptions.LambdaSubscription(this.lambdaFunction, {
          filterPolicyWithMessageBody: {
            partner: sns.FilterOrPolicy.policy({
              brand: sns.FilterOrPolicy.filter(
                sns.SubscriptionFilter.stringFilter({
                  allowlist: ['all'],
                })
              ),
            }),
          },
        })
      );

      if (props.isSnsTriggerFromEventBridge) {
        // Create event bridge rule to run every 15 mins
        const eventRuleName = `${functionName}-rule`;

        const eventRule = new events.Rule(this, `${functionName}-rule`, {
          enabled: true,
          description: `Event rule for ${topicName}`,
          ruleName: eventRuleName,
          schedule: events.Schedule.rate(
            cdk.Duration.minutes(props.eventBridgeTriggerScheduleMinutes)
          ),
        });

        // Add sns target for eb rule
        eventRule.addTarget(
          new targets.SnsTopic(topic, {
            message: events.RuleTargetInput.fromObject({
              partner: { brand: 'all' },
            }),
          })
        );
      }
    }
  }
}
