import * as cdk from 'aws-cdk-lib';
import { CfnOutput } from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { RuntimeFamily, Runtime } from 'aws-cdk-lib/aws-lambda';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as path from 'path';
import { Queue } from 'aws-cdk-lib/aws-sqs';
import * as iam from 'aws-cdk-lib/aws-iam';

export interface ILambdaSettings extends cdk.StackProps {
  targetEnvironment: string;
  functionName: string;
  lambdaProjectName: string;
  buildType: string;
  aspNetCoreEnvironment: string;
  isWebApi?: boolean;
  lambdaTimeoutSeconds: number;
  createFunctionUrl?: boolean;
  serviceDirectory: string;
  lambdaSettings: any;
}

export class LambdaFunctionConstruct extends Construct {
  public readonly lambdaFunction: lambda.Function;
  public readonly primaryQueue?: Queue;
  public readonly functionUrl?: string;

  constructor(scope: Construct, id: string, props: ILambdaSettings) {
    super(scope, id);

    const functionName = props.functionName;
    const localEnvironment = props.targetEnvironment;

    const serviceDirectory = path.join(
      __dirname,
      '../../..',
      props.serviceDirectory
    );

    const functionBinFolder = path.join(
      serviceDirectory,
      props.lambdaProjectName,
      `bin/${props.buildType}/net6.0`
    );

    const handler = props.isWebApi
      ? props.lambdaProjectName
      : `${props.lambdaProjectName}::${props.lambdaProjectName}.Function::FunctionHandler`;

    const defaultSettings = {
      ENVIRONMENT_NAME: `${localEnvironment}`,
      ASPNETCORE_ENVIRONMENT: `${props.aspNetCoreEnvironment}`,
      SERVICE_NAME: `${props.lambdaProjectName}`,
    };

    const lambdaSettings = { ...defaultSettings, ...props.lambdaSettings };

    this.lambdaFunction = new lambda.Function(this, `${functionName}-lambda`, {
      functionName,
      runtime: new Runtime('dotnet6', RuntimeFamily.DOTNET_CORE),
      code: lambda.Code.fromAsset(functionBinFolder),
      handler: handler,
      architecture: lambda.Architecture.X86_64,
      memorySize: 512,
      timeout: cdk.Duration.seconds(props.lambdaTimeoutSeconds),
      tracing: lambda.Tracing.ACTIVE,
      logRetention: logs.RetentionDays.TWO_WEEKS,
      environment: lambdaSettings,
    });

    if (props.createFunctionUrl) {
      // 👇 Setup lambda url
      const lambdaUrl = this.lambdaFunction.addFunctionUrl({
        authType: lambda.FunctionUrlAuthType.NONE,
      });

      this.functionUrl = lambdaUrl.url;

      // 👇 print lambda url after deployment
      new CfnOutput(this, 'FunctionUrl ', { value: lambdaUrl.url });
    }

    this.lambdaFunction.addToRolePolicy(
      new iam.PolicyStatement({
        effect: iam.Effect.ALLOW,
        actions: [
          'logs:DescribeLogGroups',
          'logs:StartQuery',
          'logs:GetQueryResults',
          'logs:GetLogEvents',
          'logs:CreateLogStream',
        ],
        resources: ['*'],
      })
    );
  }
}
