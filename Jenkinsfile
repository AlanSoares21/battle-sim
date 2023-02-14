pipeline {
	agent any
	stages {
		stage('Test') {
			steps {
				sh 'whoami'
				sh 'sudo ./dotnet-test-docker.sh'
			}
		}
	}
}
