import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { ApiDestination, Authorization, Connection,Rule,EventBus } from 'aws-cdk-lib/aws-events';
import { Queue } from 'aws-cdk-lib/aws-sqs';
import * as targets from 'aws-cdk-lib/aws-events-targets';
// import * as sqs from 'aws-cdk-lib/aws-sqs';

export interface PolicyStackProps extends cdk.StackProps
{
  environmentName: string;
  resourcePrefix: string;
};

export class CdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: PolicyStackProps) {
    super(scope, id, props);

    // Define Queues
    const policyCreatedQName = `${props?.resourcePrefix}Q${props?.environmentName}`
    const policyCreatedQ = new Queue(this, policyCreatedQName)

    const dlqName = `${props?.resourcePrefix}DLQ${props?.environmentName}`
    const dlq = new Queue(this, dlqName)
    
    // Define EventBus
    const busName = `${props?.resourcePrefix}EventBus${props?.environmentName}`
    const bus = new EventBus(this, 'PolicyCreatedEventBus')

    // Define ApiDefinition
    
    // dummy value here because target has no auth but connection requires this parameter
    // ignore or replace with a secure password/token if needed
    const apisecret = cdk.SecretValue.unsafePlainText("notused")

    const conn = new Connection(this, "connection", {
        authorization: Authorization.apiKey("notused", apisecret)
      }
    )

    const webhookUrl = '<INSERT_VALUE>' // retrieve from parameter store ?? or props
    const apiDestination = new ApiDestination(this, "PolicyCreatedApiDestination",
      {
        connection: conn,
        endpoint: webhookUrl,
        rateLimitPerSecond: 1,
      }
    )

    // Define EventRule
    const target = new targets.ApiDestination(apiDestination, {
      deadLetterQueue: dlq,
      retryAttempts: 3,
      headerParameters: {
        // https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/
        'Idempotency-Key': '$.detail.customID' // replace with desired field from your event
      }
    })

    const rule = new Rule(this, 'rule', {
      eventBus: bus,
      targets: [target]
    })

    new cdk.CfnOutput(this, 'busArn', { value: bus.eventBusArn })
    new cdk.CfnOutput(this, 'ruleArn', { value: rule.ruleArn })
    new cdk.CfnOutput(this, 'dlqArn', { value: dlq.queueArn })
    new cdk.CfnOutput(this, 'apiDestinationArn', { value: apiDestination.apiDestinationArn })
  }
}
