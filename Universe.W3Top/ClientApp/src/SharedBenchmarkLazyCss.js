import "./SharedBenchmark.css"
import React, { Component } from 'react';

export default function SharedBenchmarkLazyCss() {
    React.useEffect(() => {
        if (document && document.getElementById) {
            const body = document.getElementById("TheBody");
            if (body) {
                body.className = "TheBody";
            }
        }
    });

    return null;
}