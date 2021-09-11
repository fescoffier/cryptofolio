#!/usr/bin/env python
#

import argparse
import datetime
import logging
import json
import psycopg2
import requests
import time

COINGECKO_API = "https://api.coingecko.com/api/v3"


def fetch_tickers(args):
    logging.info("Calling Goingecko coins API using the following parameters:\n\tcoins: %s\n\tvs_currencies: %s\n\tdays: %s\n\tinterval: %s",
                 args.coins,
                 args.vs_currencies,
                 args.days,
                 args.data_interval)

    tickers = []

    for coin in args.coins.split(","):
      for vs_currency in args.vs_currencies.split(","):
        logging.info("Fetching tickers for the %s coin versus currency %s.", coin, vs_currency)
        result = requests.get(
            f"{COINGECKO_API}/coins/{coin}/market_chart?vs_currency={vs_currency}&days={args.days}&interval={args.data_interval}")
        logging.debug("Response status code: %s", result.status_code)
        logging.debug("Data: \n%s", result.json())

        response = json.loads(result.content)
        tickers.extend([{
            "asset_id": coin,
            "vs_currency_code": vs_currency,
            "timestamp": datetime.datetime.fromtimestamp(price[0] / 1000).strftime("%Y-%m-%d %H:%M:%S.%f"),
            "value": price[1]
        } for price in response["prices"]])

        logging.debug("Waiting %s seconds before the next call.", args.call_interval)
        time.sleep(int(args.call_interval))

    logging.info("Fetched %d tickers from Coingecko.", len(tickers))
    return tickers


def main(args, loglevel):
    logging.basicConfig(format="%(levelname)s: %(message)s", level=loglevel)

    logging.info("Collecting currency tickers from the Coingecko API.")

    tickers = fetch_tickers(args)
    commands = [f"insert into \"data\".\"asset_ticker\" (\"timestamp\",\"asset_id\",\"vs_currency_id\",\"value\") \
                values ('{t['timestamp']}','{t['asset_id']}',(select id from \"data\".\"currency\" where \"code\" = '{t['vs_currency_code'].lower()}'),{t['value']}) \
                on conflict (\"timestamp\",\"asset_id\",\"vs_currency_id\") do update set \"value\" = {t['value']};"
                for t in tickers]
    logging.info("Opening the database connection.")
    with psycopg2.connect(database=args.database, user=args.user, password=args.password, host=args.host, port=args.port) as con:
        logging.info("Database connection opened.")
        with con.cursor() as cur:
            logging.info("Executing SQL commands.")
            for command in commands:
                logging.debug("Executing SQL command: %s", command)
                cur.execute(command)
            logging.info("Committing to database.")
            con.commit()
            logging.info("Committed to database.")

    logging.info("Currency tickers collected from the Coingecko API.")


# Standard boilerplate to call the main() function to begin
# the program.
if __name__ == '__main__':
    parser = argparse.ArgumentParser(
        description="Collects historical asset ticker from the Coingecko API.",
        epilog="As an alternative to the commandline, params can be placed in a file, one per line, and specified on the commandline like '%(prog)s @params.conf'.",
        fromfile_prefix_chars='@')
    parser.add_argument(
        "-c",
        "--coins",
        help="the coins list",
        metavar="bitcoin,ethereum")
    parser.add_argument(
        "-vs",
        "--vs_currencies",
        help="the versus currencies list",
        metavar="usd,eur")
    parser.add_argument(
        "-ds",
        "--days",
        help="data up to number of days ago",
        metavar="1,14,30,max")
    parser.add_argument(
        "-di",
        "--data_interval",
        help="the data interval",
        metavar="daily")
    parser.add_argument(
        "-ci",
        "--call_interval",
        help="the API call interval in seconds",
        metavar="10")
    parser.add_argument(
        "-ht",
        "--host",
        help="the database server host",
        metavar="localhost")
    parser.add_argument(
        "-pt",
        "--port",
        help="the database server port",
        metavar="5432")
    parser.add_argument(
        "-d",
        "--database",
        help="the database",
        metavar="cryptofolio")
    parser.add_argument(
        "-u",
        "--user",
        help="the database user",
        metavar="cryptofolio")
    parser.add_argument(
        "-p",
        "--password",
        help="the database password",
        metavar="Pass@word1")
    parser.add_argument(
        "-v",
        "--verbose",
        help="increase output verbosity",
        action="store_true")
    args = parser.parse_args()

    # Setup logging
    if args.verbose:
        loglevel = logging.DEBUG
    else:
        loglevel = logging.INFO

    main(args, loglevel)
