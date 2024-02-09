#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { ServiceCdkStack } from '../lib/service-cdk';
import { IStackSettings } from '../lib/service-types';

const action: string = process.env.cdk_action || 'deploy';
const accountId: string = process.env.aws_account_id || '301804962855';
const service_key: string = process.env.service_key || 'portal-api';
const ecr_repository_name: string =
  process.env.ecr_repository_name || 'portal-api';
const target_environment: string = process.env.target_environment || 'local';
const build_run_number: string = process.env.run_number || '-1';
const build_Type: string = process.env.build_Type || 'Release';
const aspnet_environment: string =
  process.env.aspnet_environment || 'Development';

const stackSettings: IStackSettings = {
  //  ecr_repository_name: ecr_repository_name,
  build_run_number: build_run_number,
  /*  fargateSettings: {
    targetEnvironment: target_environment,
    aspNetEnvironment: 'Development',
    serviceKey: service_key,
    serviceName: 'PortalApi',
    layer: 'WebApi',
    awsRegion: 'ap-southeast-2',
    hostEnvironmentType: 'ECS',
    applicationName: 'PortalApi',
  },*/
  lambdaProjects: [
    {
      targetEnvironment: target_environment,
      aspNetCoreEnvironment: aspnet_environment,
      buildType: build_Type,
      lambdaProjectName: 'PortalApi',
      functionName: `portal-api-${target_environment}`,
      isQueueTrigger: false,
      eventBridgeTriggerScheduleMinutes: -1,
      isWebApi: true,
      lambdaTimeoutSeconds: 30,
      createFunctionUrl: true,
    },
    {
      targetEnvironment: target_environment,
      aspNetCoreEnvironment: aspnet_environment,
      buildType: build_Type,
      lambdaProjectName: 'BillingUsageService',
      functionName: `billing-usage-service-${target_environment}`,
      isSnsTrigger: true,
      isSnsTriggerFromEventBridge: true,
      eventBridgeTriggerScheduleMinutes: 60,
      lambdaTimeoutSeconds: 900,
    },
  ],
  env: {
    account: accountId,
    region: 'ap-southeast-2',
  },
};

const app = new cdk.App();

const stackName = `PortalServiceCdkStack${target_environment}`;
new ServiceCdkStack(app, stackName, stackSettings);
