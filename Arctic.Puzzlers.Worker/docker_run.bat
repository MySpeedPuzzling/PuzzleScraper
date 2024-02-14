docker run -v "$HOME/.config/gcloud:C:\Users\buffa\AppData\Roaming\gcloud" \
  -v /gcp/config/logs \
  --env CLOUDSDK_CONFIG=/gcp/config \
  --env GOOGLE_APPLICATION_CREDENTIALS=/gcp/config/application_default_credentials.json \
  ...