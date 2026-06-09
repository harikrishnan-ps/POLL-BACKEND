pipeline {
    agent any
 
    environment {
        CONTAINER_NAME = 'poll-backend'
        IMAGE_NAME = 'poll-api'
        NETWORK_NAME = 'app-net'
        PORT_MAPPING = '5050:8080' // Using 5050 to avoid conflicts with existing APIs mapping to container port 8080
    }
 
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
 
        stage('Build Docker Image') {
            steps {
                bat "docker build --no-cache -t ${IMAGE_NAME}:latest -t ${IMAGE_NAME}:${BUILD_NUMBER} ."
            }
        }
 
        stage('Deploy Container') {
            steps {
                script {
                    // Ensure Docker network exists (shared with frontend)
                    bat "docker network create ${NETWORK_NAME} 2>nul || ver >nul"
                   
                    // Stop and remove existing container if running
                    bat "docker stop ${CONTAINER_NAME} 2>nul || ver >nul"
                    bat "docker rm ${CONTAINER_NAME} 2>nul || ver >nul"
                   
                    // Launch new container using Windows Batch line continuation
                    bat """
                        docker run -d ^
                            --name ${CONTAINER_NAME} ^
                            --network ${NETWORK_NAME} ^
                            -p ${PORT_MAPPING} ^
                            ${IMAGE_NAME}:latest
                    """
                }
            }
        }
    }
 
    post {
        success {
            echo "Backend pipeline completed successfully!"
        }
        failure {
            echo "Backend pipeline failed. Please check the logs."
        }
    }
}
