#!/bin/bash

# export resourceGroupName=$resourceGroupName
# export location=$location
# export workloadName=$workloadName
# export workloadEnv=$workloadEnv



projectPath="Azure.Lcm.Web"   # Path to your .NET project
outputPath="publish-content"          # Path to publish the project
configuration="Release"         # Configuration to build
publishZip="publish.zip"        # Name of the zip file to create

echo "Building project in '$projectPath' with configuration '$configuration'"
dotnet publish $projectPath -c $configuration -o $outputPath

echo "Zipping the published files"
zip -r $publishZip "$outputPath/*"
