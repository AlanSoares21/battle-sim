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
				ApiUrl = credentials("ApiUrl")
				WsUrl = credentials("WsUrl")
			}
			steps {
				sh './build-server.sh'
				sh './build-webclient.sh'
			}
		}
	}
}
