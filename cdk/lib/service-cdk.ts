import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
/*
import {
  IFargateSettings,
  FargateServiceConstruct,
} from './fargate-service-construct';
*/
import * as iam from 'aws-cdk-lib/aws-iam';
import { LambdaFunctionConstruct } from './lambda-function-construct';
import { IStackSettings } from './service-types';

/* Fargate mode
export class ServiceCdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: IStackSettings) {
    super(scope, id, props);

    const fargateSettings: IFargateSettings = {
      targetEnvironment: props.fargateSettings.targetEnvironment,
      aspNetEnvironment: props.fargateSettings.aspNetEnvironment,
      serviceKey: props.fargateSettings.serviceKey,
      serviceName: props.fargateSettings.serviceName,
      layer: props.fargateSettings.layer,
      awsRegion: props.fargateSettings.awsRegion,
      hostEnvironmentType: props.fargateSettings.hostEnvironmentType,
      applicationName: props.fargateSettings.applicationName,
      ecr_repository_name: props.ecr_repository_name,
      build_run_number: props.build_run_number,
    };

    const fargateServiceConstruct = new FargateServiceConstruct(
      this,
      `${props.fargateSettings.serviceName}Fargate`,
      fargateSettings
    );
  }
}
*/

/* Lambda mode */
export class ServiceCdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: IStackSettings) {
    super(scope, id, props);

    const outerConstruct = this;
    props.lambdaProjects.forEach(function (lambdaSettings) {
      const lambdaFunctionConstruct = new LambdaFunctionConstruct(
        outerConstruct,
        `${lambdaSettings.lambdaProjectName}Lambda`,
        lambdaSettings
      );

      lambdaFunctionConstruct.lambdaFunction.addToRolePolicy(
        new iam.PolicyStatement({
          effect: iam.Effect.ALLOW,
          actions: [
            'secretsManager:DescribeSecret',
            'secretsManager:GetSecretValue',
            'secretsManager:CreateSecret',
            'secretsManager:UpdateSecret',
            'secretsManager:DeleteSecret',
            'secretsManager:PutSecretValue',
            's3:PutObject',
            's3:GetObject',
            's3:GetObjectVersion',
            's3:DeleteObject',
            'logs:DescribeLogGroups',
            'logs:StartQuery',
            'logs:GetQueryResults',
            'logs:GetLogEvents',
            'logs:CreateLogStream',
          ],
          resources: ['*'],
        })
      );
    });
  }
}
