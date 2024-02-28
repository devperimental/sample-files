#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { EvbDlqStack } from '../lib/evb-dlq-stack';
import { IStackSettings } from '../lib/service-types';

const accountId: string = process.env.aws_account_id || '301804962855';
const target_environment: string = process.env.target_environment || 'local';
const aspNetCoreEnvironment: string =
  process.env.aspNetCoreEnvironment || 'Local';
const build_run_number: string = process.env.run_number || '-1';

const stackSettings: IStackSettings = {
  resourcePrefix: 'FailedMessageProcessor',
  build_run_number: build_run_number,
  environmentName: target_environment,
  aspNetCoreEnvironment: aspNetCoreEnvironment,
  env: {
    account: accountId,
    region: 'ap-southeast-2',
  },
};

const app = new cdk.App();

const stackName = `EvbDlqStack${target_environment}`;
new EvbDlqStack(app, stackName, stackSettings);
