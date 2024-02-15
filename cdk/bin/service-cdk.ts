#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';

import { IStackSettings } from '../lib/service-types';
import { EventBridgeCdkStack } from '../lib/eventbridge-construct';

const accountId: string = process.env.aws_account_id || '301804962855';
const target_environment: string = process.env.target_environment || 'local';
const build_run_number: string = process.env.run_number || '-1';

const stackSettings: IStackSettings = {
  resourcePrefix: 'PolicyCreated',
  build_run_number: build_run_number,
  environmentName: target_environment,
  env: {
    account: accountId,
    region: 'ap-southeast-2',
  }
};

const app = new cdk.App();

const stackName = `EventBridgeCdkStack${target_environment}`;
new EventBridgeCdkStack(app, stackName, stackSettings);
