import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { IStackSettings } from './service-types';
import { EventBus } from 'aws-cdk-lib/aws-events';
import * as ssm from 'aws-cdk-lib/aws-ssm';
import { Queue } from 'aws-cdk-lib/aws-sqs';
import {
  ApiDestination,
  Authorization,
  Connection,
  Rule,
} from 'aws-cdk-lib/aws-events';
import * as targets from 'aws-cdk-lib/aws-events-targets';

export class EventBridgeCdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: IStackSettings) {
    super(scope, id, props);

    const outerConstruct = this;

    // 1 EventBus Creation - To receive put events from source
    const busName = `${props?.resourcePrefix}EventBus${props?.environmentName}`;
    const bus = new EventBus(this, busName);

    // 2 Save the EventBusName to SSM
    new ssm.StringParameter(this, 'ssm-eventBusName', {
      parameterName: `/settings/${props.environmentName}/${props?.resourcePrefix}/eventBusName`,
      stringValue: bus.eventBusName,
    });
    
    // 3 Create a queue to serve as the DLQ for failed messages
    const messageFailedQueueName = `${props?.resourcePrefix}FailureQueue${props?.environmentName}`;
    const messageFailedQueue = new Queue(this, messageFailedQueueName, {
        queueName: messageFailedQueueName,
    });

    // 4 Save the DLQ name to SSM
    new ssm.StringParameter(this, 'ssm-failureQueueName', {
      parameterName: `/settings/${props.environmentName}/${props?.resourcePrefix}/failureQueueUrl`,
      stringValue: messageFailedQueue.queueUrl,
    });

    // 5 Get the apiKeyName from ssm
    const apiKeyName = ssm.StringParameter.fromStringParameterName(
      this,
      'ssm-apiKeyName',
      `/settings/${props.environmentName}/${props?.resourcePrefix}/apiKeyName`
    ).stringValue;

    // 5 Get the apiKeyName from ssm
    const apiKeyArn = ssm.StringParameter.fromStringParameterName(
      this,
      'ssm-apiKeyArn',
      `/settings/${props.environmentName}/${props?.resourcePrefix}/apiKeyArn`
    ).stringValue;
    
    // 5 Configure the Connection
    const connectionName = `${props?.resourcePrefix}Connection${props?.environmentName}`;
    const apisecret = cdk.SecretValue.secretsManager(apiKeyArn);
    const connection = new Connection(this, connectionName, {
      authorization: Authorization.apiKey(apiKeyName, apisecret),
      connectionName: connectionName,
    });
 
    // 6 Get the webhook url from ssm
    const webhookUrl = ssm.StringParameter.fromStringParameterName(
      this,
      'ssm-webhookUrl',
      `/settings/${props.environmentName}/${props?.resourcePrefix}/webhookUrl`
    ).stringValue;

    // 7 Create the ApiDestination
    const apiDestinationName = `${props?.resourcePrefix}ApiConnection${props?.environmentName}`;
    const apiDestination = new ApiDestination(this, apiDestinationName, {
      connection: connection,
      endpoint: webhookUrl,
      rateLimitPerSecond: 1,
      apiDestinationName: apiDestinationName,
    });

    // 8 Configure the targets
    const target = new targets.ApiDestination(apiDestination, {
      deadLetterQueue: messageFailedQueue,
      retryAttempts: 3,
      headerParameters: {
        // https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/
        IdempotencyKey: '$.detail.idempotencyKey', // replace with desired field from your event
        RequestId: '$.detail.requestId',
      },
    });

    // 9 Create the Define EventRule
    const ruleName = `${props?.resourcePrefix}RuleName${props?.environmentName}`;
    const rule = new Rule(this, ruleName, {
      eventBus: bus,
      eventPattern: { account: [this.account] },
      targets: [target],
      ruleName: ruleName,
    });
  }
}
