## Assessment Grievance App

<br>

**What is this?** This is an app that allows for digitization of a real property assessment grievance process managed by a municipal government in New York State. For more info, please see [this link](https://derek-baker.github.io/).

<br>

**How do I run it?** This app is dependent on a couple of services from specific vendors. Some of the services below could be swapped out easily in favor of other options, but swapping out other services (such object storage and secret management) would require changes to the application. 


| Service            | Purpose                  |
|--------------------|--------------------------|
| GCP Cloud Run      | Container-based compute  |
| GCP Cloud Storage  | Object storage           |
| GCP Secret Manager | Secret management        |
| MongoDB Atlas      | Managed database cluster |
| Sendgrid           | Outbound email           |
| Google             | CAPCHAs                  |

`*GCP == Google Cloud Platform`

For detailed steps on configuring the above services, please see [these docs](./src/_Docs/setup.md).

<br>
