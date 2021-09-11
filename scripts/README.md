# Fetching historical data

## On the development environment

Execute this command to fetch currencies historical data from Fixer :

```bash
python collect_historical_currency_tickers.py \
  --access_key $FIXER_APIKEY \
  --base USD \
  --symbols EUR,JPY,GBP,CHF,CAD \
  --years 2020,2019 \
  --host localhost \
  --port 55432 \
  --database cryptofolio \
  --user cryptofolio \
  --password Pass@word1 \
  -v
```

⚠️ You need a [Fixer](https://fixer.io) API key.

Execute this command to fetch assets historical data from Coingecko :

```bash
python collect_historical_asset_tickers.py \
  --coins bitcoin,ethereum,binancecoin,cardano,ripple,dogecoin,polkadot,uniswap,solana,litecoin,chainlink,theta-token,stellar,internet-computer,vechain \
  --vs_currencies usd,eur,jpy,gbp,chf,cad \
  --days max \
  --data_interval daily \
  --call_interval 10 \
  --host localhost \
  --port 55432 \
  --database cryptofolio \
  --user cryptofolio \
  --password Pass@word1 \
  -v
```

⚠️ Coingecko API limit is 50 calls/minute using a free pricing plan.

## On the production environment

Execute this command to fetch currencies historical data from Fixer :

```bash
FIXER_APIKEY=$(kubectl get secret fixer -o jsonpath="{.data.apiKey}" | base64 -d) POSTGRES_PASSWORD=$(kubectl get secret postgres-passwords -o jsonpath="{.data.postgresql-password}" | base64 -d) collect_historical_currency_tickers.py \
  --access_key $FIXER_APIKEY \
  --base USD \
  --symbols EUR,JPY,GBP,CHF,CAD \
  --years 2020,2019 \
  --host localhost \
  --port 5432 \
  --database cryptofolio \
  --user cryptofolio \
  --password $POSTGRES_PASSWORD \
  -v
```

Execute this command to fetch assets historical data from Coingecko :

```bash
POSTGRES_PASSWORD=$(kubectl get secret postgres-passwords -o jsonpath="{.data.postgresql-password}" | base64 -d) collect_historical_asset_tickers.py \
  --coins bitcoin,ethereum,binancecoin,cardano,ripple,dogecoin,polkadot,uniswap,solana,litecoin,chainlink,theta-token,stellar,internet-computer,vechain \
  --vs_currencies usd,eur,jpy,gbp,chf,cad \
  --days max \
  --data_interval daily \
  --call_interval 10 \
  --host localhost \
  --port 5432 \
  --database cryptofolio \
  --user cryptofolio \
  --password $POSTGRES_PASSWORD \
  -v
```

⚠️ You will need the access to the production K8S cluster and having the Postgres service port forwarded on your machine.