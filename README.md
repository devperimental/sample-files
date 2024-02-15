# sample-files

## Setup

### Creation

aws ssm put-parameter ^
--name "/settings/local/PolicyCreated/apiKeyName" ^
--value "my-test-api-key-name" ^
--type String ^
--region ap-southeast-2

aws ssm put-parameter ^
--name "/settings/local/PolicyCreated/webhookUrl" ^
--value "https://www.toptal.com/developers/postbin/b/1707976132223-1221578652039" ^
--type String ^
--region ap-southeast-2

aws secretsmanager create-secret --name TestSecretNameA --secret-string TestSecretValueB --region ap-southeast-2

aws ssm put-parameter ^
--name "/settings/local/PolicyCreated/apiKeyArn" ^
--value "arn:aws:secretsmanager:<region-id>:<account-id>:secret:TestSecretNameA-<key>" ^
--type String ^
--region ap-southeast-2

### Retrieval

aws ssm get-parameter --name "/settings/local/PolicyCreated/apiKeyName" --region ap-southeast-2
aws ssm get-parameter --name "/settings/local/PolicyCreated/webhookUrl" --region ap-southeast-2
aws ssm get-parameter --name "/settings/local/PolicyCreated/apiKeyArn" --region ap-southeast-2

aws secretsmanager get-secret-value --secret-id "arn:aws:secretsmanager:<region-id>:<account-id>:secret:TestSecretNameA-<key>" --region ap-southeast-2

### Clean up

aws ssm delete-parameter --name "/settings/local/PolicyCreated/apiKeyName" --region ap-southeast-2
aws ssm delete-parameter --name "/settings/local/PolicyCreated/webhookUrl" --region ap-southeast-2
aws ssm delete-parameter --name "/settings/local/PolicyCreated/apiKeyArn" --region ap-southeast-2

aws secretsmanager delete-secret --secret-id "arn:aws:secretsmanager:<region-id>:<account-id>:secret:TestSecretName-<key>" --region ap-southeast-2 --force-delete-without-recovery
