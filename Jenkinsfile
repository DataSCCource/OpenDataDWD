pipeline {
	agent any

	stages {
		stage('build') {
			steps {
				gerritReview score:-1
				bat "\"${NuGet}\" restore OpenDataDWD/OpenDataDWD.Tests/packages.config -PackagesDirectory packages"
				bat "\"${tool 'MSBuild'}\" OpenDataDWD/OpenDataDWD.sln -t:Build /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"
			}
		}
//		stage('test') {
//			steps {
//				bat "\"${NUnit}\" OpenDataDWD/OpenDataDWD.Tests/bin/Release/OpenDataDWD.Tests.dll --result=TestR.xml;format=nunit2"
//			}
//		}
	}

	post {
		success {
			gerritReview score:1
		}
		failure {
			gerritReview score:-1
		}
	}
}