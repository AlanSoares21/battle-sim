pipeline {
	agent any
	stages {
		stage('Test') {
			steps {
				sh './dotnet-test-docker.sh'
			}
		}
		stage('Build') {
			environment {
				ApiUrl = ${env.ApiUrl}
				WsUrl = ${env.WsUrl}
			}
			steps {
				sh './build-server.sh'
				sh './build-webclient.sh'
			}
		}
	}
}
