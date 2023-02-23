#!/bin/bash
install-jenkins() {
	# copied from https://www.jenkins.io/doc/book/installing/linux/
	echo "Loaging keys..."
	curl -fsSL https://pkg.jenkins.io/debian-stable/jenkins.io.key | sudo tee \
	/usr/share/keyrings/jenkins-keyring.asc > /dev/null
	echo deb [signed-by=/usr/share/keyrings/jenkins-keyring.asc] \
	https://pkg.jenkins.io/debian-stable binary/ | sudo tee \
	/etc/apt/sources.list.d/jenkins.list > /dev/null
	sudo apt update
	echo "Installing Jenkins..."
	sudo apt install jenkins
	echo "Installing java..."
	sudo apt install openjdk-11-jre
	java -version
	echo "Enabling and starting jenkins..."
	sudo systemctl enable jenkins
	sudo systemctl start jenkins
}

install-docker() {
	sudo apt install docker.io
	sudo systemctl enable docker
}

enable-docker-to-jenkins() {
	if groups jenkins | grep docker ; then
		echo "jenkins is enabled to use docker"
	else
		echo "jenkins is not enbled to use docker"
		sudo usermod -a -G docker jenkins
		echo "now jenkins is enabled to user docker"
	fi
}

install-nginx() {
	sudo apt install nginx
}

sudo apt update

if jenkins --version ; then
	echo "Jenkins is installed"
else
	echo "Installing Jenkins"
	install-jenkins
	echo "Jenkins installed and running"
fi

if docker --version ; then
	echo "Docker is installed"
else
	echo "Installing Docker"
	install-docker
	echo "Docker installed and running"
fi

enable-docker-to-jenkins

if nginx -v ; then
	echo "Nginx installed"
else
	echo "Installing nginx"
	install-nginx
	echo "Nginx installed and running"
fi

# handling nginx configuration
CopyNginxConfig=$1
if [ -n $CopyNginxConfig -a $CopyNginxConfig -eq "y"  ]; then
	echo "Updating nginx configuration files..."
	sudo cp -r nginx/* /etc/nginx/
	echo -e "Nginx configuration files updated \n Run nginx -t to test the cofiguration \n Run nginx -s reload to reload the configuration"
else
	echo "Nginx configuration files dont changed."
fi
