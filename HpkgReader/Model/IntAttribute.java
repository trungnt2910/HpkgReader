/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

package org.haiku.pkg.model;

import com.google.common.base.Preconditions;
import org.haiku.pkg.AttributeContext;

import java.math.BigInteger;

/**
 * <p>This attribute is an integral numeric value.  Note that the format specifies either a signed or unsigned value,
 * but this concrete subclass of @{link Attribute} serves for both the signed and unsigned cases.</p>
 */

public class IntAttribute extends Attribute {

    private final BigInteger numericValue;

    public IntAttribute(AttributeId attributeId, BigInteger numericValue) {
        super(attributeId);
        Preconditions.checkNotNull(numericValue);
        this.numericValue = numericValue;
    }

    @Override
    public BigInteger getValue(AttributeContext context) {
        return numericValue;
    }

    @SuppressWarnings("RedundantIfStatement") // was auto generated
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        IntAttribute that = (IntAttribute) o;

        if (!numericValue.equals(that.numericValue)) return false;

        return true;
    }

    @Override
    public int hashCode() {
        return numericValue.hashCode();
    }

    @Override
    public AttributeType getAttributeType() {
        return AttributeType.INT;
    }

    @Override
    public String toString() {
        return String.format("%s : %s",super.toString(), numericValue.toString());
    }

}
