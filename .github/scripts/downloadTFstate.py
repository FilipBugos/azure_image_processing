import os
from azure.storage.blob import BlobServiceClient, ContainerClient

def download_tfstate_blob(connection_string, container_name, local_directory='./', blob_prefix=''):

    blob_service_client = BlobServiceClient.from_connection_string(connection_string)
    container_client = blob_service_client.get_container_client(container_name)

    blobs = container_client.list_blobs(name_starts_with=blob_prefix)

    tfstate_blob_found = False

    for blob in blobs:
        blob_name = blob.name
        if blob_name.endswith(".tfstate"):
            tfstate_blob_found = True
            blob_file_name = os.path.basename(blob_name)
            local_file_path = os.path.join(local_directory, blob_file_name)

            with open(local_file_path, "wb") as data:
                blob_data = container_client.download_blob(blob_name)
                data.write(blob_data.readall())
                print(f"Blob '{blob_name}' downloaded successfully and saved as '{local_file_path}'.")

    if not tfstate_blob_found:
        print(f"No .tfstate blob found in the specified Azure Blob Storage container with prefix: {blob_prefix}")


# Load Azure credentials from environment variables
azure_connection_string = os.environ.get("AZURE_STORAGE_CONNECTION_STRING")
azure_container_name = os.environ.get("AZURE_STORAGE_CONTAINER_NAME")

# Local directory to save the downloaded .tfstate file(s)
local_directory = "./"

# Blob prefix (optional)
blob_prefix = "terraformState"

# Check if all required environment variables are set
if not all([azure_connection_string , azure_container_name]):
    print("Azure credentials are not set. Make sure to set AZURE_STORAGE_ACCOUNT_NAME, AZURE_STORAGE_ACCOUNT_KEY, and AZURE_STORAGE_CONTAINER_NAME environment variables.")
else:
    # Download the .tfstate blob(s) from Azure Blob Storage to the local directory
    download_tfstate_blob(azure_connection_string, azure_container_name, local_directory, blob_prefix)
