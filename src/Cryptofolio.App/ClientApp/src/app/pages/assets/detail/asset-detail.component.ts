import { AfterViewInit, Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { AssetsService } from "../assets.service";

@Component({
  selector: "app-asset-details",
  templateUrl: "asset-detail.component.html",
  styleUrls: ["asset-detail.component.scss"]
})
export class AssetDetailComponent implements OnInit {
  public asset: any;

  constructor(private route: ActivatedRoute, private service: AssetsService) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      if (params["id"]) {
        this.service.get(params["id"]).subscribe(asset => {
          this.asset = asset;
          this.loadTradingViewScript(asset.symbol.toUpperCase() + "USD");
        });
      }
    });
  }

  private loadTradingViewScript(pair: string) {
    if (!document.getElementById("tv_script")) {
      const node = document.createElement('script');
      node.id = "tv_script"
      node.src = "https://s3.tradingview.com/tv.js";
      node.type = 'text/javascript';
      node.async = false;
      node.onload = e => this.createTradingViewChart(pair);
      document.getElementsByTagName('head')[0].appendChild(node);
    } else {
      setTimeout(() => this.createTradingViewChart(pair), 1000);
    }
  }

  private createTradingViewChart(pair: string) {
    new (window as any).TradingView.widget({
      "autosize": true,
      "symbol": `COINBASE:${pair}`,
      "interval": "D",
      "timezone": "Etc/UTC",
      "theme": "dark",
      "style": "1",
      "locale": "en",
      "toolbar_bg": "#f1f3f6",
      "enable_publishing": false,
      "allow_symbol_change": false,
      "container_id": "tradingview_chart"
    });
  }
}
