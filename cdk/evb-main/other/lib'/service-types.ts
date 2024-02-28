import * as cdk from 'aws-cdk-lib';
import { Queue } from 'aws-cdk-lib/aws-sqs';
import { EventBus } from 'aws-cdk-lib/aws-events';

export interface IStackSettings extends cdk.StackProps {
  resourcePrefix: string;
  build_run_number: string;
  environmentName: string;
  lambdaProjects: Array<ILambdaSettings>;
}

export interface ILambdaSettings extends cdk.StackProps {
  targetEnvironment: string;
  functionName: string;
  lambdaProjectName: string;
  buildType: string;
  aspNetCoreEnvironment: string;
  isQueueTrigger?: boolean;
  isWebApi?: boolean;
  lambdaTimeoutSeconds: number;
  createFunctionUrl?: boolean;
  serviceDirectory: string;
  lambdaSettings: any;
}

export interface IEventRuleSettings extends cdk.StackProps {
  dlq?: Queue;
  eventBus: EventBus;
  webhookUrl: string;
  resourcePrefix: string;
  target_environment: string;
  account: cdk.Stack['account'];
}
