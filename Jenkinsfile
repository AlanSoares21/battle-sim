pipeline {
	agent any
	stages {
		stage('Test') {
			steps {
				sh './dotnet-test-docker.sh'
			}
		}
	}
}
