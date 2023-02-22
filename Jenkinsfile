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
				sh './Clients/Web/build-docker-image.sh'
			}
		}
		stage('Deploy') {
			environment {
				ApiUrl = credentials("ApiUrl")
				WsUrl = credentials("WsUrl")
				SiteUrl = credentials("SiteUrl")
				JwtSecret = credentials("JwtSecret")
			}
			steps {
				sh './run-server.sh'
				sh './run-webclient.sh'
			}
		}
	}
}
