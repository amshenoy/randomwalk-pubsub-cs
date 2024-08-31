"use client"

import React, { useEffect, useState } from 'react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { StockServiceClient } from './generated/stock.client'
import { StockStreamRequest, StockPriceResponse, RequestType } from './generated/stock'

// Assuming the gRPC-Web server is running on localhost:8080
const client = new StockServiceClient('http://localhost:5243')

export default function StockChart() {
  const [priceData, setPriceData] = useState<{ time: string; price: number }[]>([])
  const [symbol, setSymbol] = useState<string>('AAPL')

  useEffect(() => {
    const request = new StockStreamRequest()
    request.setSymbolsList([symbol])
    request.setType(RequestType.SUBSCRIBE)

    const stream = client.priceStream()

    stream.on('data', (response: StockPriceResponse) => {
      setPriceData((prevData) => {
        const newData = [...prevData, { time: new Date().toLocaleTimeString(), price: response.getPrice() }]
        return newData.slice(-20) // Keep only the last 20 data points
      })
    })

    stream.on('error', (err: Error) => {
      console.error('Error:', err)
    })

    stream.on('end', () => {
      console.log('Stream ended')
    })

    stream.write(request)

    return () => {
      const unsubscribeRequest = new StockStreamRequest()
      unsubscribeRequest.setSymbolsList([symbol])
      unsubscribeRequest.setType(RequestType.UNSUBSCRIBE)
      stream.write(unsubscribeRequest)
      stream.cancel()
    }
  }, [symbol])

  const handleSymbolChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSymbol(event.target.value.toUpperCase())
  }

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Stock Price Chart</h1>
      <div className="mb-4">
        <label htmlFor="symbol" className="mr-2">
          Stock Symbol:
        </label>
        <input
          type="text"
          id="symbol"
          value={symbol}
          onChange={handleSymbolChange}
          className="border border-gray-300 rounded px-2 py-1"
        />
      </div>
      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={priceData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis />
          <Tooltip />
          <Legend />
          <Line type="monotone" dataKey="price" stroke="#8884d8" name={symbol} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  )
}
