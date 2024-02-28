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

    // 1 EventBus Creation - To receive put events from source
    const busName = `${props?.resourcePrefix}EventBus${props?.environmentName}`;
    const bus = new EventBus(this, busName);

    // 2 Event Rule Lambda Creation - To send put events to the event bus
    const eventBusName = bus.eventBusName;
    const eventRuleLambdaEnvironmentVariables = {
      EVENT_BUS_NAME: eventBusName,
    };

    const eventRuleLambdaSettings = props.lambdaProjects.filter(function (
      item
    ) {
      return item['lambdaProjectName'] == 'EventRuleLambda';
    });

    eventRuleLambdaSettings[0].lambdaSettings =
      eventRuleLambdaEnvironmentVariables;

    const eventRuleLambdaConstruct = new LambdaFunctionConstruct(
      outerConstruct,
      `${eventRuleLambdaSettings[0].lambdaProjectName}Lambda`,
      eventRuleLambdaSettings[0]
    );

    // 3 Event Rule DLQ Lambda Creation - To receive failed attempts to reach the api destination
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

    // 4 External API Lambda Creation - To simulate an external api url for the Api Destination connection
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

    // 5 Event Rule Creation - Event bridge event rule to connect to event bus and api destination
    const webhookUrl =
      externalAPILambdaConstruct.functionUrl + 'notify/partner/testPartner'; // retrieve from parameter store ?? or props

    const eventRuleSettings: IEventRuleSettings = {
      dlq: eventRuleDLQLambdaConstruct.primaryQueue,
      eventBus: bus,
      webhookUrl: webhookUrl,
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
