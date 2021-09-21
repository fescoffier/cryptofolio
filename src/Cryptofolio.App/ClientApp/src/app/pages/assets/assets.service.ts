import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

const COINGECKO_API = "https://api.coingecko.com/api/v3";

@Injectable()
export class AssetsService {
  constructor(private http: HttpClient) {}

  get(id: string): Observable<any> {
    return this.http.get<any>(`${COINGECKO_API}/coins/${id}`);
  }
}