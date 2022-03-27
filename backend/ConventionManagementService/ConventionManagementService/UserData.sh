#!/bin/bash

#dotnet core app
sudo yum install -y git
rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-3.1
cd home/ec2-user
git clone https://github.com/Julandia/cms.git
git checkout aws-deploy
cd /home/ec2-user/cms/backend/ConventionManagementService
dotnet clean ConventionManagementService.sln
dotnet build ConventionManagementService.sln
cd ConventionManagementService/bin/Debug/netcoreapp3.1
dotnet ConventionManagementService.dll &

#vue frontend
curl --silent --location https://rpm.nodesource.com/setup_16.x | sudo bash -
sudo yum install -y nodejs
cd /home/ec2-user/vue-frontend
npm install
npm run serve -- --port 80 &
