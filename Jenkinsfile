// Declarative Jenkinsfile for Learning Content Validator
// Designed for execution on a Windows-based Jenkins build agent with .NET SDK installed.
// Demonstrates enterprise CI/CD standards: clean checkout, package restoration, strict compilation,
// automated test execution with TRX logging, artifact publishing, and notification hooks.

pipeline {
    agent {
        // Run on an agent node labeled 'windows' (or 'any' depending on your cluster configuration)
        label 'windows'
    }

    options {
        // Abort build if execution exceeds 30 minutes to prevent hung build executors
        timeout(time: 30, unit: 'MINUTES')
        // Retain only the 10 most recent builds to manage Jenkins master disk storage
        buildDiscarder(logRotator(numToKeepStr: '10'))
        // Timestamp every console log line for debugging build bottlenecks
        timestamps()
        // Disable concurrent builds on the same branch
        disableConcurrentBuilds()
    }

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO = '1'
        CONFIGURATION = 'Release'
        SOLUTION_PATH = 'LearningContentValidator.sln'
        PUBLISH_DIR = "${WORKSPACE}\\artifacts\\publish"
        TEST_RESULTS_DIR = "${WORKSPACE}\\artifacts\\test-results"
    }

    stages {
        stage('Checkout') {
            steps {
                echo '=== Stage 1: SCM Checkout ==='
                // Checks out the latest source revision from the configured Git repository
                checkout scm
            }
        }

        stage('Environment Diagnostic') {
            steps {
                echo '=== Stage 2: Verify .NET Environment ==='
                // Outputs .NET SDK and installed runtimes for build traceability and triage
                bat 'dotnet --info'
            }
        }

        stage('Restore Dependencies') {
            steps {
                echo '=== Stage 3: Restore NuGet Dependencies ==='
                // Restores project dependencies using the solution lock/cache
                bat "dotnet restore ${SOLUTION_PATH} --verbosity normal"
            }
        }

        stage('Build Solution') {
            steps {
                echo '=== Stage 4: Compile Solution (Release Mode) ==='
                // Compiles with --no-restore for speed; enforces clean compilation with zero warnings-as-errors if desired
                bat "dotnet build ${SOLUTION_PATH} --configuration ${CONFIGURATION} --no-restore"
            }
        }

        stage('Run Automated Tests') {
            steps {
                echo '=== Stage 5: Execute NUnit Test Suite ==='
                // Runs tests in Release mode without rebuilding.
                // Logs results using the Visual Studio TRX logger into the artifacts directory.
                // Jenkins MSTest / TRX plugin consumes this file to plot pass/fail trends.
                bat """
                    if not exist "${TEST_RESULTS_DIR}" mkdir "${TEST_RESULTS_DIR}"
                    dotnet test tests/LearningContentValidator.Tests/LearningContentValidator.Tests.csproj ^
                        --configuration ${CONFIGURATION} ^
                        --no-build ^
                        --logger "trx;LogFileName=test_results.trx" ^
                        --results-directory "${TEST_RESULTS_DIR}"
                """
            }
        }

        stage('Publish Application') {
            steps {
                echo '=== Stage 6: Publish WPF Application Executable ==='
                // Compiles and publishes the WPF desktop application as a standalone or framework-dependent package
                bat """
                    if not exist "${PUBLISH_DIR}" mkdir "${PUBLISH_DIR}"
                    dotnet publish src/LearningContentValidator.App/LearningContentValidator.App.csproj ^
                        --configuration ${CONFIGURATION} ^
                        --no-build ^
                        --output "${PUBLISH_DIR}"
                """
            }
        }

        stage('Archive Build Artifacts') {
            steps {
                echo '=== Stage 7: Archive Build and Test Artifacts ==='
                // Archives the published binaries and test reports in Jenkins for download and audit
                archiveArtifacts artifacts: 'artifacts/**/*', fingerprint: true, allowEmptyArchive: false
            }
        }
    }

    post {
        always {
            echo '=== Post Execution: Publish Test Reports ==='
            // Ingests the TRX test execution report so Jenkins can visualize pass/fail graphs and test history.
            // If any test failed, this step ensures the failure is highlighted in the build summary.
            mstest testResultsFile: 'artifacts/test-results/*.trx', keepLongStdio: true
        }

        success {
            echo 'SUCCESS: All tests passed and artifacts generated successfully.'
        }

        unstable {
            echo 'UNSTABLE: Build succeeded but one or more automated tests failed.'
        }

        failure {
            echo 'FAILURE: Compilation or pipeline execution encountered a fatal error.'
        }
    }
}

