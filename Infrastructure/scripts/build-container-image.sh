#!/bin/bash


echo "Login to Azure Container Registry"
accessToken=$(az acr login --name $registryURI --expose-token --output tsv --query accessToken)
docker login $registryURI --username 00000000-0000-0000-0000-000000000000 --password $accessToken

echo "Building Images with Tag '${imageName}:${tag}'"
docker build -t ${registryURI}/${imageName}:${tag} -f Dockerfile .

echo "Pushing to '$registryURI'"
docker push ${registryURI}/${imageName}:${tag}
