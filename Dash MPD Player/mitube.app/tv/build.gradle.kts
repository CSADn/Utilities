plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.mitube.tv"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.mitube.tv"
        minSdk = 24
        targetSdk = 35
        versionCode = 1
        versionName = "1.0"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }
}

dependencies {
    implementation(project(":core"))
    implementation("androidx.core:core-ktx:1.12.0")
    implementation("androidx.leanback:leanback:1.0.0")
    implementation("androidx.appcompat:appcompat:1.6.1")
    implementation("androidx.security:security-crypto:1.1.0-alpha06")
    implementation("com.github.bumptech.glide:glide:4.16.0")
    implementation("androidx.fragment:fragment-ktx:1.7.1")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.7.0")
}
