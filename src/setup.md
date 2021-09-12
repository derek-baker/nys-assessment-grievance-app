- GCP
	- Create GCP project
	- Enable Sendgrid: Marketplace > Sendgrid Email API > Free > Register with SendGrid
		- Log into Sendgrid at https://app.sendgrid.com/login
		- select MFA option > verify identity using selected factor
		- Create verified sender or from verify domain
		- API Keys > Create API Key > Name the key > Select desired permissions
	- Enable Secret Manager
		- Security > Secret Manager
		- https://cloud.google.com/secret-manager/docs/configuring-secret-manager
		- Enable > Create Secret > Fill in options > click 'Create Secret' button

	- Assigning Permissions To Service Account Used To Run App:
		- IAM > Select Principal checkbox > Edit Principal:
			- Assign Cloud Storage Roles: Add 'Storage Admin'
			- Assign Secret Manger Roles: Add 'Secret Manager Accessor'
	
	- Authenticating to GCP services while running locally:
		- "When running on Google Cloud Platform, no action needs to be taken to authenticate (between services such as Cloud Run and Cloud Storage). Otherwise, the simplest way of authenticating your API calls is to download a service account JSON file then set the GOOGLE_APPLICATION_CREDENTIALS environment variable to refer to it. The credentials will automatically be used to authenticate."
		- IAM > Service Accounts > Select desired service account that will be used to run the Cloud Run app
			- Keys > Create Key > JSON > [System.Environment]::SetEnvironmentVariable('GOOGLE_APPLICATION_CREDENTIALS','C:\<Path>\<To>\<ServiceAccountKey>\<KEY_FILE_NAME>.json',[System.EnvironmentVariableTarget]::User)
			- https://cloud.google.com/iam/docs/creating-managing-service-account-keys


- MongoDB: https://www.mongodb.com/try 
	- Create account 
	- Atlas 
	- Get Started Free > Create account
	- Log into, and nav to Atlas
	- Build a Database > Free/Basic > Select a cloud provider and region (Consider the effect of geography on latency) > M0 (free) > Create cluster
	- Security > Database Access > Add New Database User > Configure as to your preferences (password is easiest)
	- Connect > Allow Access From Anywhere
	 
	