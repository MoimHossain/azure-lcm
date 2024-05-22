using 'main.bicep'


var appname = readEnvironmentVariable('APP_NAME')
var appEnv = readEnvironmentVariable('APP_ENV')
var tag = readEnvironmentVariable('tag')
var image = readEnvironmentVariable('imageName')

param containerappsjobname = '${appname}job'
param uamiName = '${appname}-uami-${appEnv}'
param containerRegistryName = '${appname}contregistry${appEnv}'
param acaEnvName = '${appname}-appenv-${appEnv}'
param tagName = '${tag}'
param imageName = '${image}'
param appInsightName = '${appname}-appinsights-${appEnv}'
