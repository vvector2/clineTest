#!/bin/bash
set -euo pipefail

bucket="${S3_POSTERS_BUCKET:-clinetest-posters}"
region="${AWS_DEFAULT_REGION:-us-east-1}"

if awslocal s3api head-bucket --bucket "$bucket" >/dev/null 2>&1; then
    echo "S3 bucket '$bucket' already exists."
else
    if [ "$region" = "us-east-1" ]; then
        awslocal s3api create-bucket --bucket "$bucket"
    else
        awslocal s3api create-bucket \
            --bucket "$bucket" \
            --create-bucket-configuration "LocationConstraint=$region"
    fi

    echo "Created S3 bucket '$bucket' in region '$region'."
fi

awslocal s3api put-bucket-policy \
    --bucket "$bucket" \
    --policy "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Sid\":\"PublicReadPosters\",\"Effect\":\"Allow\",\"Principal\":\"*\",\"Action\":\"s3:GetObject\",\"Resource\":\"arn:aws:s3:::$bucket/*\"}]}"

echo "Applied public read policy to S3 bucket '$bucket'."
