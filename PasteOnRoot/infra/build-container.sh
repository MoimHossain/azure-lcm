#!/bin/bash

# export resourceGroupName=$resourceGroupName
# export location=$location
# export workloadName=$workloadName
# export workloadEnv=$workloadEnv
# export registryURI=$registryURI
# export imageName=$imageName
# export imageTag=$imageTag


echo "Login to Azure Container Registry"
accessToken=$(az acr login --name $registryURI --expose-token --output tsv --query accessToken)
docker login $registryURI --username 00000000-0000-0000-0000-000000000000 --password $accessToken

echo "Building Images with Tag '${imageName}:${imageTag}'"
docker build -t ${registryURI}/${imageName}:${imageTag} -f Dockerfile .

echo "Pushing to '$registryURI'"
docker push ${registryURI}/${imageName}:${imageTag}
