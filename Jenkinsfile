pipeline {
	agent any
	stages {
		stage('Test') {
			steps {
				sh 'whoami'
				sh './dotnet-test-docker.sh'
			}
		}
	}
}
