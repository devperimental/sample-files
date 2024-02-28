import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { IStackSettings } from './service-types';
import { Topic } from 'aws-cdk-lib/aws-sns';
import { Rule, Schedule, RuleTargetInput } from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';
import { LambdaSubscription } from 'aws-cdk-lib/aws-sns-subscriptions';
import { ILambdaSettings, LambdaFunctionConstruct } from './lambda-construct';

export class EvbDlqStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: IStackSettings) {
    super(scope, id, props);

    const snsTopicName = `${props?.resourcePrefix}SNSTopic${props?.environmentName}`;

    const snsTopic = new Topic(this, `${snsTopicName}`, {
      displayName: 'Lambda subscription topic',
    });

    const retryLambdaSettings: ILambdaSettings = {
      targetEnvironment: props.environmentName,
      functionName: 'RetryLambda',
      lambdaProjectName: 'FailedMessageProcessorLambda',
      buildType: 'Release',
      aspNetCoreEnvironment: props.aspNetCoreEnvironment,
      isWebApi: false,
      lambdaTimeoutSeconds: 300,
      createFunctionUrl: false,
      serviceDirectory: 'src/code/FailedMessageProcessorPOC',
      lambdaSettings: {},
    };

    const retryLambdaName = `${props?.resourcePrefix}RetryLambda${props?.environmentName}`;
    const retryLambdaConstruct = new LambdaFunctionConstruct(
      this,
      `${retryLambdaName}`,
      retryLambdaSettings
    );

    // Subscribes the consumer lambda to the topic
    snsTopic.addSubscription(
      new LambdaSubscription(retryLambdaConstruct.lambdaFunction)
    );

    const ruleName = `${props?.resourcePrefix}RuleName${props?.environmentName}`;
    const rule = new Rule(this, ruleName, {
      description: `This rule is for schedule to snsTopic ${snsTopicName}`,
      schedule: Schedule.rate(cdk.Duration.hours(1)),
      ruleName: ruleName,
    });

    rule.addTarget(
      new targets.SnsTopic(snsTopic, {
        message: RuleTargetInput.fromText(
          `Schedule triggered from eventbridge`
        ),
      })
    );

    rule.applyRemovalPolicy(cdk.RemovalPolicy.DESTROY);
  }
}
