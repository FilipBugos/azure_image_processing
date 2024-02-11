import os
from azure.storage.blob import BlobServiceClient, ContainerClient

def upload_tf_file_to_blob_storage(connection_string, container_name, parent_folder_path, blob_prefix=''):

    blob_service_client = BlobServiceClient.from_connection_string(connection_string)
    container_client = blob_service_client.get_container_client(container_name)

    tf_file_found = False

    for file in os.listdir(parent_folder_path):
        if file.endswith(".tfstate"):
            tf_file_found = True
            local_file_path = os.path.join(parent_folder_path, file)
            blob_name = os.path.join(blob_prefix, file).replace("\\", "/")

            blob_client = container_client.get_blob_client(blob_name)
            if blob_client.exists():
                print(f"Blob '{blob_name}' already exists in Azure Blob Storage. Deleting the existing blob.")
                blob_client.delete_blob()

            with open(local_file_path, "rb") as data:
                container_client.upload_blob(name=blob_name, data=data)
                print(f"File '{file}' uploaded successfully to Azure Blob Storage.")

    if not tf_file_found:
        print(f"No .tfstate file found in the specified parent folder path: {parent_folder_path}")


# Load Azure credentials from environment variables
azure_connection_string = os.environ.get("AZURE_STORAGE_CONNECTION_STRING")
azure_container_name = os.environ.get("AZURE_STORAGE_CONTAINER_NAME")

# Parent folder path containing .tf file(s)
parent_folder_path = "./.."

# Blob prefix (optional)
blob_prefix = "terraformState"

# Check if all required environment variables are set
if not all([azure_connection_string , azure_container_name]):
    print("Azure credentials are not set. Make sure to set AZURE_STORAGE_CONNECTION_STRING, and AZURE_STORAGE_CONTAINER_NAME environment variables.")
else:
    # Upload the .tf file(s) from the parent folder to Azure Blob Storage
    upload_tf_file_to_blob_storage(azure_connection_string, azure_container_name, parent_folder_path, blob_prefix)
