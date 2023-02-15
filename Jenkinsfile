pipeline {
	agent any
	stages {
		stage('Test') {
			steps {
				sh './dotnet-test-docker.sh'
			}
		}
		stage('Build') {
			steps {
				sh './build-server.sh'
				sh './build-webclient.sh'
			}
		}
	}
}
