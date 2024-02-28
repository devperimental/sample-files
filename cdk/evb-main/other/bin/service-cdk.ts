#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { PocCdkStack } from '../lib/cdk-stack';
import { IStackSettings } from '../lib/service-types';

const action: string = process.env.cdk_action || 'deploy';
const accountId: string = process.env.aws_account_id || '301804962855';
const target_environment: string = process.env.target_environment || 'local';
const build_run_number: string = process.env.run_number || '-1';
const build_Type: string = process.env.build_Type || 'Release';
const aspnet_environment: string =
  process.env.aspnet_environment || 'Development';
const src_dir: string = process.env.src_dir || 'src/code/ApiDestinationPOC';

const stackSettings: IStackSettings = {
  resourcePrefix: 'EventRulePOC',
  build_run_number: build_run_number,
  lambdaProjects: [
    {
      targetEnvironment: target_environment,
      aspNetCoreEnvironment: aspnet_environment,
      buildType: build_Type,
      lambdaProjectName: 'EventRuleLambda',
      functionName: `event-rule-source-${target_environment}`,
      isQueueTrigger: true,
      lambdaTimeoutSeconds: 300,
      createFunctionUrl: true,
      serviceDirectory: src_dir,
      lambdaSettings: {},
    },
    {
      targetEnvironment: target_environment,
      aspNetCoreEnvironment: aspnet_environment,
      buildType: build_Type,
      lambdaProjectName: 'EventRuleDLQLambda',
      functionName: `event-rule-failed-${target_environment}`,
      isQueueTrigger: true,
      lambdaTimeoutSeconds: 300,
      serviceDirectory: src_dir,
      lambdaSettings: {},
    },
    {
      targetEnvironment: target_environment,
      aspNetCoreEnvironment: aspnet_environment,
      buildType: build_Type,
      lambdaProjectName: 'ExternalApi',
      functionName: `external-api-${target_environment}`,
      isWebApi: true,
      createFunctionUrl: true,
      lambdaTimeoutSeconds: 300,
      serviceDirectory: src_dir,
      lambdaSettings: {},
    },
  ],
  environmentName: target_environment,
  env: {
    account: accountId,
    region: 'ap-southeast-2',
  },
};

const app = new cdk.App();

const stackName = `PocCdkStack${target_environment}`;
new PocCdkStack(app, stackName, stackSettings);
