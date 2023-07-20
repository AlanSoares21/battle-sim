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
                                SiteUrl = credentials("SiteUrl")
				AssetsUrl = credentials("AssetsUrl")
			}
			steps {
				sh './build-server.sh'
				sh './build-webclient.sh'
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
