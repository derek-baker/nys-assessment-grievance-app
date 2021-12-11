- Google Cloud Platform (GCP)
	- Create GCP project
		- You can DuckDuckGo this one, right?
	
	- Enable Sendgrid Within GCP: Marketplace > Sendgrid Email API > Free > Register with SendGrid
		- Log into Sendgrid at https://app.sendgrid.com/login
		- select MFA option > verify identity using selected factor
		- Create verified sender or from verify domain
		- API Keys > Create API Key > Name the key > Select desired permissions
	
	- Enable Secret Manager:
		- Security > Secret Manager
		- Enable > Create Secret > Fill in options > click 'Create Secret' button
		
	- Assigning Permissions To Service Account Used To Run App:
		- IAM > Select Principal checkbox > Edit Principal:
			- Assign Cloud Storage Roles: Add 'Storage Admin'
			- Assign Secret Manger Roles: Add 'Secret Manager Accessor'
	
	- Authenticating to GCP services while running locally:
		- "When running on Google Cloud Platform, no action needs to be taken to authenticate (between services such as Cloud Run and Cloud Storage). Otherwise, the simplest way of authenticating your API calls is to download a service account JSON file then set the GOOGLE_APPLICATION_CREDENTIALS environment variable to refer to it. The credentials will automatically be used to authenticate."
		- IAM > Service Accounts > Select desired service account that will be used to run the Cloud Run app
			- Keys > Create Key > JSON > Download key
			- Locally, create an operating system env var that points to the key downloaded in the previous step. If you're on Windows, the command might look like this:
				``` powershell
				[System.Environment]::SetEnvironmentVariable('GOOGLE_APPLICATION_CREDENTIALS','<Drive>:\<Path>\<To>\<ServiceAccountKey>\<KEY_FILE_NAME>.json', [System.EnvironmentVariableTarget]::User)
				```
			


- MongoDB: https://www.mongodb.com/try 
	- Create account 
	- Atlas 
	- Get Started Free > Create account
	- Log in, and nav to Atlas
	- Build a Database > Free/Basic > Select a cloud provider and region (Consider the effect of geography on latency) > M0 (free) > Create cluster
	- Security > Database Access > Add New Database User > Configure as to your preferences (password is easiest)
	- Connect > Allow Access From Anywhere (You'll want to restrict this later)

- ReCapcha: https://www.google.com/recaptcha/admin/site
	 
	