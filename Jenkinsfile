pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
    }

    stages {

        stage('Checkout') {
            steps {
                git branch: 'master',
                    url: 'https://github.com/sharugesanr/CareerCrafter.git',
                    credentialsId: 'github-credentials'
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore CareerCrafter.slnx'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build CareerCrafter.slnx --configuration Release --no-restore'
            }
        }

        stage('Test') {
            steps {
                bat 'dotnet test CareerCrafter.slnx --configuration Release --no-build --verbosity normal'
            }
        }
    }

    post {
        success {
            echo 'CareerCrafter pipeline completed successfully!'
        }

        failure {
            echo 'Pipeline failed. Check the console output.'
        }
    }
}