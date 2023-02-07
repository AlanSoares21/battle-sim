pipeline {
	agent { docker { image 'mcr.microsoft.com/dotnet/sdk:7.0' } }
	stages {
		stage('Test') {
			steps {
				sh 'test.sh'
			}
		}
		stage('Build') {
			steps {
				sh 'build-server.sh'
				sh 'build-webclient.sh'
			}
		}
	}
}
