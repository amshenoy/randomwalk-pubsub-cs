"use client"

import React, { useEffect, useState, useRef } from 'react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { createGrpcWebTransport, createClient } from '@connectrpc/connect-web'
import { StockService } from './generated/stock_connectweb' // Adjust the import based on the generated files
import { StockStreamRequest, StockPriceResponse, RequestType } from './generated/stock_pb'

export default function StockChart() {
  const [priceData, setPriceData] = useState<{ time: string; price: number }[]>([])
  const [symbol, setSymbol] = useState<string>('AAPL')

  // Use a ref to keep track of the current stream
  const streamRef = useRef<any>(null)

  useEffect(() => {
    const transport = createGrpcWebTransport({
      baseUrl: 'http://localhost:5243', // Your gRPC-Web server URL
    })

    const client = createClient(StockService, transport)

    const request = new StockStreamRequest()
    request.symbols.push(symbol)
    request.type = RequestType.SUBSCRIBE

    const stream = client.priceStream(request, {
      onMessage: (response: StockPriceResponse) => {
        setPriceData((prevData) => {
          const newData = [...prevData, { time: new Date().toLocaleTimeString(), price: response.price }]
          return newData.slice(-20) // Keep only the last 20 data points
        })
      },
      onEnd: (code, message, trailers) => {
        if (code !== 0) {
          console.error('Stream ended with error:', message)
        } else {
          console.log('Stream ended successfully')
        }
      },
    })

    // Store the stream in the ref to be able to cancel it later
    streamRef.current = stream

    return () => {
      // Unsubscribe when the component unmounts or the symbol changes
      if (streamRef.current) {
        const unsubscribeRequest = new StockStreamRequest()
        unsubscribeRequest.symbols.push(symbol)
        unsubscribeRequest.type = RequestType.UNSUBSCRIBE
        client.priceStream(unsubscribeRequest, {
          onEnd: () => {
            console.log('Unsubscribed from the stream')
          },
        })
        streamRef.current.close()
      }
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
