import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import {
  ApiDestination,
  Authorization,
  Connection,
  Rule,
} from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';
import { IEventRuleSettings } from './service-types';

export class EventRuleConstruct extends Construct {
  public readonly rule: Rule;
  public readonly ruleArn: string;

  constructor(scope: Construct, id: string, props: IEventRuleSettings) {
    super(scope, id);

    // Define Connection
    const apisecret = cdk.SecretValue.unsafePlainText('notused');
    const connectionName = `${props?.resourcePrefix}Connection${props?.target_environment}`;
    const conn = new Connection(this, connectionName, {
      authorization: Authorization.apiKey('notused', apisecret),
      connectionName: connectionName,
    });

    // Define ApiDefinition
    const webhookUrl = props.webhookUrl + 'partner/testPartner'; // retrieve from parameter store ?? or props
    const apiDestinationName = `${props?.resourcePrefix}ApiConnection${props?.target_environment}`;
    const apiDestination = new ApiDestination(this, apiDestinationName, {
      connection: conn,
      endpoint: webhookUrl,
      rateLimitPerSecond: 1,
      apiDestinationName: apiDestinationName,
    });

    const target = new targets.ApiDestination(apiDestination, {
      deadLetterQueue: props.dlq,
      retryAttempts: 3,
      headerParameters: {
        // https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/
        IdempotencyKey: '$.detail.idempotencyKey', // replace with desired field from your event
        RequestId: '$.detail.requestId',
      },
    });

    // Define EventRule
    const ruleName = `${props?.resourcePrefix}RuleName${props?.target_environment}`;
    const rule = new Rule(this, ruleName, {
      eventBus: props.eventBus,
      eventPattern: { account: [props.account] },
      targets: [target],
      ruleName: ruleName,
    });

    //new cdk.CfnOutput(this, 'busArn', { value: bus.eventBusArn })
    new cdk.CfnOutput(this, 'ruleArn', { value: rule.ruleArn });
    new cdk.CfnOutput(this, 'apiDestinationArn', {
      value: apiDestination.apiDestinationArn,
    });
  }
}
