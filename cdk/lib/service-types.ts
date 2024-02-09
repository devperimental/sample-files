import * as cdk from 'aws-cdk-lib';

export interface IStackSettings extends cdk.StackProps {
  ecr_repository_name?: string;
  build_run_number: string;
  fargateSettings?: IFargateConfig;
  lambdaProjects: Array<ILambdaSettings>;
}

export interface IFargateConfig extends cdk.StackProps {
  targetEnvironment: string;
  aspNetEnvironment: string;
  serviceKey: string;
  serviceName: string;
  layer: string;
  awsRegion: string;
  hostEnvironmentType: string;
  applicationName: string;
}
/*
export interface ILambdaConfig extends cdk.StackProps {
  serviceKey: string;
  targetEnvironment: string;
  aspNetCoreEnvironment: string;
  buildType: string;
  lambdaProjectName: string;
  functionName: string;
  isQueueTrigger?: boolean;
  isWebApi?: boolean;
  lambdaTimeoutSeconds: number;
  createFunctionUrl: boolean;
}
*/
export interface ILambdaSettings extends cdk.StackProps {
  targetEnvironment: string;
  functionName: string;
  lambdaProjectName: string;
  buildType: string;
  aspNetCoreEnvironment: string;
  isQueueTrigger?: boolean;
  isSnsTrigger?: boolean;
  isSnsTriggerFromEventBridge?: boolean;
  eventBridgeTriggerScheduleMinutes: number;
  isWebApi?: boolean;
  lambdaTimeoutSeconds: number;
  createFunctionUrl?: boolean;
}
