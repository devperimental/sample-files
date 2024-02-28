import * as cdk from 'aws-cdk-lib';

export interface IStackSettings extends cdk.StackProps {
  resourcePrefix: string;
  build_run_number: string;
  environmentName: string;
  aspNetCoreEnvironment: string;
}
