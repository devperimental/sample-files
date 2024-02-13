import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { IStackSettings, IEventRuleSettings } from './service-types';
import { LambdaFunctionConstruct } from './lambda-function-construct';
import * as iam from 'aws-cdk-lib/aws-iam';
import { EventBus } from 'aws-cdk-lib/aws-events';
import { EventRuleConstruct } from './event-rule-construct';

export class PocCdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: IStackSettings) {
    super(scope, id, props);

    const outerConstruct = this;

    // Event Rule Lambda
    //////////////////////////
    const eventRuleLambdaSettings = props.lambdaProjects.filter(function (
      item
    ) {
      return item['lambdaProjectName'] == 'EventRuleLambda';
    });

    const eventRuleLambdaConstruct = new LambdaFunctionConstruct(
      outerConstruct,
      `${eventRuleLambdaSettings[0].lambdaProjectName}Lambda`,
      eventRuleLambdaSettings[0]
    );

    eventRuleLambdaConstruct.lambdaFunction.addToRolePolicy(
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

    // Event Rule DLQ Lambda
    //////////////////////////
    const eventRuleDLQLambdaSettings = props.lambdaProjects.filter(function (
      item
    ) {
      return item['lambdaProjectName'] == 'EventRuleDLQLambda';
    });
    const eventRuleDLQLambdaConstruct = new LambdaFunctionConstruct(
      outerConstruct,
      `${eventRuleDLQLambdaSettings[0].lambdaProjectName}Lambda`,
      eventRuleDLQLambdaSettings[0]
    );

    eventRuleDLQLambdaConstruct.lambdaFunction.addToRolePolicy(
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

    // Define EventBus
    const busName = `${props?.resourcePrefix}EventBus${props?.environmentName}`;
    const bus = new EventBus(this, busName);

    // External API Lambda
    //////////////////////////
    const externalAPILambdaSettings = props.lambdaProjects.filter(function (
      item
    ) {
      return item['lambdaProjectName'] == 'ExternalApi';
    });
    const externalAPILambdaConstruct = new LambdaFunctionConstruct(
      outerConstruct,
      `${externalAPILambdaSettings[0].lambdaProjectName}Lambda`,
      externalAPILambdaSettings[0]
    );

    externalAPILambdaConstruct.lambdaFunction.addToRolePolicy(
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

    // need this for IEventRuleSettings
    const eventRuleSettings: IEventRuleSettings = {
      dlq: eventRuleDLQLambdaConstruct.primaryQueue,
      eventBus: bus,
      webhookUrl: externalAPILambdaConstruct.functionUrl,
      resourcePrefix: props.resourcePrefix,
      target_environment: props.environmentName,
      account: this.account,
    };

    const eventRule = new EventRuleConstruct(
      outerConstruct,
      `${props.resourcePrefix}`,
      eventRuleSettings
    );
  }
}
