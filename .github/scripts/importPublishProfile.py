import os
from azure.identity import DefaultAzureCredential
import requests

# Set environment variables for Azure credentials
#os.environ["AZURE_TENANT_ID"] = ".."
#os.environ["AZURE_CLIENT_ID"] = ".."
#os.environ["AZURE_CLIENT_SECRET"] = ".."

#os.environ["AZURE_SUBS_ID"] = ".."
#os.environ["WEB_APP_NAME"] = ".."
#os.environ["WEB_APP_RESOURCE_GROUP_NAME"] = ".."

# Set the name of the Azure web app
web_app_name = os.environ.get("WEB_APP_NAME")

def get_access_token():
    # Use DefaultAzureCredential to get access token
    credential = DefaultAzureCredential()
    access_token = credential.get_token("https://management.azure.com/.default").token
    return access_token

def get_web_app_publish_profiles(access_token):
    # Construct the URL to get the publish profiles for the web app
    subscriptionId = os.environ.get("AZURE_SUBS_ID")
    resourceGroupName = os.environ.get("WEB_APP_RESOURCE_GROUP_NAME")
    web_app_name = os.environ.get("WEB_APP_NAME")

    url = f"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web/sites/{web_app_name}/publishxml?api-version=2022-03-01"
    # url = f"https://{web_app_name}.scm.azurewebsites.net/api/publishprofiles"

    # Add the access token to the request headers
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/xml",
    }

    # Make the HTTP request to get the publish profiles
    response = requests.post(url, headers=headers)

    if response.status_code == 200:
        return response.text
    else:
        print(f"Error: {response.status_code} - {response.text}")
        return None

def main():
    access_token = get_access_token()

    if access_token:
        publish_profiles = get_web_app_publish_profiles(access_token)

        if publish_profiles:
            print("Publish Profiles:")
            print("success")
        else:
            print("Failed to retrieve publish profiles.")
    else:
        print("Failed to obtain access token.")


main()