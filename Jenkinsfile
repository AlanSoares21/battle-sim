pipeline {
	agent { docker { image 'mcr.microsoft.com/dotnet/sdk:7.0' } }
	stages {
		stage('test') {
			sh test.sh
		}
		stage('build') {
			sh build-server.sh
			sh build-webclient.sh
		}
		stag('deploy') {
		}
	}
}
